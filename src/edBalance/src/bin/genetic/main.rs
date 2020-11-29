mod letters;

use ed_balance::models::{get_score, print_letters, Digraphs, DynError, Settings};
use itertools::Itertools;
use letters::Letters;
use std::{cmp::Ordering, collections::HashMap, process};
use structopt::StructOpt;

fn main() {
    let args = Settings::from_args();
    if let Err(e) = run(&args) {
        eprintln!("Calculations failed: {:#?}", e);
        process::exit(1);
    }
}

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

pub fn run(settings: &Settings) -> Result<(), DynError> {
    let digraphs = Digraphs::load(&settings.digraphs)?;

    let mut population: HashMap<_, _> = (0..settings.population_size)
        .into_iter()
        .map(|_| {
            let letters = Letters::new(&digraphs);
            let version = letters.version.clone();
            (version, letters)
        })
        .collect();

    for _ in 0..settings.generations_count {
        population = process(&population, &digraphs, &settings);
    }

    for item in population.iter().take(10).map(|(_, item)| item) {
        print_letters(&item.left, &item.right, item.left_score, item.right_score);
    }

    Ok(())
}

fn score_desc_cmp(b: &Box<Letters>, a: &Box<Letters>) -> Ordering {
    get_score(a.left_score, a.right_score)
        .partial_cmp(&get_score(b.left_score, b.right_score))
        .unwrap()
}

fn process(
    population: &HashMap<Box<String>, Box<Letters>>,
    digraphs: &Digraphs,
    settings: &Settings,
) -> HashMap<Box<String>, Box<Letters>> {
    let mutants: Vec<_> = population
        .iter()
        .flat_map(|(_, parent)| {
            (0..settings.children_count)
                .map(|_| parent.mutate(settings.mutations_count, &digraphs))
                .collect::<Vec<_>>()
        })
        .collect();

    // todo: check that boxing prevents from creating new instances
    let mut all: Vec<_> = population
        .iter()
        .map(|(_, parent)| parent)
        .chain(mutants.iter())
        .collect();

    all.sort_by(|a, b| score_desc_cmp(a, b));

    let mut children: Vec<_> = all
        .iter()
        .take(settings.population_size)
        .group_by(|x| x.parent_version.clone())
        .into_iter()
        .flat_map(|(parent_version, group)| {
            group.tuple_windows().map(move |(a, b)| {
                let parent = population.get(&parent_version.clone()).unwrap();
                let mutations: Vec<_> = a
                    .mutations
                    .iter()
                    .chain(b.mutations.iter())
                    .unique()
                    .collect();

                parent.apply(&mutations, &digraphs)
            })
        })
        .collect();

    children.sort_by(score_desc_cmp);

    children
        .into_iter()
        .take(settings.population_size)
        .map(|x| (x.version.clone(), x))
        .collect::<HashMap<_, _>>()
}
