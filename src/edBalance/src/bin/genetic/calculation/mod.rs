mod letters;

use ed_balance::models::{get_imbalance, print_letters, Digraphs, DynError, Settings};
use itertools::Itertools;
use letters::{Letters, LettersCollection, LettersSP};
use std::cmp::Ordering;

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

pub fn run(settings: &Settings) -> Result<(), DynError> {
    let digraphs = Digraphs::load(&settings.digraphs)?;

    let mut population: Vec<_> = (0..settings.population_size)
        .into_iter()
        .map(|_| Letters::new(&digraphs))
        .collect();

    for _ in 0..settings.generations_count {
        population = process(&mut population, &digraphs, &settings);
        if population.len() == 0 {
            panic!("All died!");
        }
    }

    for item in population.iter().take(10) {
        print_letters(&item.left, &item.right, item.left_score, item.right_score);
    }

    Ok(())
}

// todo: better scoring
fn score_cmp(a: &LettersSP, b: &LettersSP) -> Ordering {
    let a_imbalance = get_imbalance(a.left_score, a.right_score);
    let b_imbalance = get_imbalance(b.left_score, b.right_score);
    let a_total = a.left_score + a.right_score;
    let b_total = b.left_score + b.right_score;

    (b_total / b_imbalance)
        .partial_cmp(&(a_total / a_imbalance))
        .unwrap()
}

fn process(
    population: &mut LettersCollection,
    digraphs: &Digraphs,
    settings: &Settings,
) -> LettersCollection {
    let mut mutants: Vec<_> = population
        .iter()
        .flat_map(|parent| {
            (0..settings.children_count)
                .map(|_| parent.mutate(settings.mutations_count, &digraphs))
                .collect::<Vec<_>>()
        })
        .collect();

    mutants.append(population);
    mutants.sort_by(|a, b| score_cmp(a, b));

    let mut children: Vec<_> = mutants
        .into_iter()
        .group_by(|x| x.parent_version.clone())
        .into_iter()
        .flat_map(|(_, group)| {
            let copy: Vec<_> = group.collect();
            if copy.len() == 1 {
                return copy;
            }

            copy.iter()
                .tuple_windows()
                .map(|(a, b)| a.cross(&b.mutations, &digraphs))
                .collect::<Vec<_>>()
        })
        .into_iter()
        .unique() // todo: exclude duplicates
        .collect();

    children.sort_by(score_cmp);

    children
        .into_iter()
        .take(settings.population_size)
        .collect()
}
