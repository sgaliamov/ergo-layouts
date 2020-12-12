mod letters;

use ed_balance::models::get_imbalance;
use ed_balance::models::{print_letters, Digraphs, DynError, Settings};
use itertools::Itertools;
use letters::{Letters, LettersCollection, LettersPointer};
use rayon::prelude::*;
use scoped_threadpool::Pool;
use std::{cmp::Ordering, sync::mpsc::channel};

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

pub fn run(settings: &Settings) -> Result<(), DynError> {
    let digraphs = Digraphs::load(&settings.digraphs)?;
    let mut pool = Pool::new(num_cpus::get() as u32);

    let mut population: Vec<_> = (0..settings.population_size)
        .into_iter()
        .map(|_| Letters::new(&digraphs))
        .collect();

    // todo: print progress

    (0..settings.generations_count).for_each( |_| {
        population = process(&mut pool, &mut population, &digraphs, &settings);
        if population.len() == 0 {
            panic!("All died!");
        }
    });

    for item in population.iter().take(10) {
        print_letters(&item.left, &item.right, item.left_score, item.right_score);
    }

    Ok(())
}

// todo: better scoring
fn score_cmp(a: &LettersPointer, b: &LettersPointer) -> Ordering {
    let a_imbalance = get_imbalance(a.left_score, a.right_score);
    let b_imbalance = get_imbalance(b.left_score, b.right_score);
    let a_total = a.left_score + a.right_score;
    let b_total = b.left_score + b.right_score;

    (b_total / b_imbalance)
        .partial_cmp(&(a_total / a_imbalance))
        .unwrap()
}

fn cross(collection: LettersCollection, digraphs: &Digraphs) -> LettersCollection {
    if collection.len() == 1 {
        return collection;
    }

    collection
        .iter()
        .tuple_windows()
        .map(|(a, b)| a.cross(&b.mutations, digraphs))
        .collect()
}

fn process(
    pool: &mut Pool,
    population: &mut LettersCollection,
    digraphs: &Digraphs,
    settings: &Settings,
) -> LettersCollection {
    let mut mutants: LettersCollection = population
        .par_iter()
        .flat_map(|parent| {
            (0..settings.children_count)
                .map(|_| parent.mutate(settings.mutations_count, &digraphs))
                .collect::<LettersCollection>()
        })
        .collect();

    mutants.append(population);
    mutants.sort_by(|a, b| score_cmp(a, b));

    let (sender, receiver) = channel();

    pool.scoped(|scoped| {
        mutants
            .into_iter()
            .group_by(|x| x.parent_version.clone())
            .into_iter()
            .for_each(|(_, group)| {
                let copy: LettersCollection = group.collect();
                let sender = sender.clone();

                scoped.execute(move || {
                    let result = cross(copy, digraphs);
                    sender.send(result).unwrap();
                });
            });
    });

    // todo: exclude duplicates

    let mut children: LettersCollection = receiver
        .iter()
        .flat_map(|x: LettersCollection| x)
        .unique()
        .collect();

    children.sort_by(score_cmp);

    children
        .into_iter()
        .take(settings.population_size)
        .collect()
}
