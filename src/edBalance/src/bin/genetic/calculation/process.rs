use super::letters::{LettersCollection, LettersPointer};
use ed_balance::models::{get_score, Digraphs, Settings};
use itertools::Itertools;
use rayon::prelude::*;
use scoped_threadpool::Pool;
use std::{cmp::Ordering, collections::HashSet, sync::mpsc::channel};

pub fn run(
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

    let mut set = HashSet::new();
    while let Ok(item) = receiver.try_recv() {
        set.extend(item);
    }

    let mut children: LettersCollection = set.into_iter().collect();
    children.sort_by(score_cmp);
    children
        .into_iter()
        .take(settings.population_size)
        .collect()
}

fn score_cmp(a: &LettersPointer, b: &LettersPointer) -> Ordering {
    let a_total = get_score(a.left_score, a.right_score);
    let b_total = get_score(b.left_score, b.right_score);

    b_total.partial_cmp(&a_total).unwrap()
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
