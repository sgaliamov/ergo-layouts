use super::letters::{LettersCollection, LettersPointer};
use ed_balance::models::{get_score, Context};
use itertools::Itertools;
use rayon::prelude::*;
use std::cmp::Ordering;

pub fn run(population: &mut LettersCollection, context: &Context) -> Result<LettersCollection, ()> {
    let mut mutants: LettersCollection = population
        .into_par_iter()
        .flat_map(|parent| {
            (0..context.children_count)
                .map(|_| parent.mutate(context))
                .collect::<LettersCollection>()
        })
        .collect();

    mutants.append(population);

    let children: LettersCollection = mutants
        .into_iter()
        .unique()
        .sorted_by(score_cmp)
        .group_by(|x| x.parent_version.clone())
        .into_iter()
        .map(|(_, group)| group.collect())
        .collect::<Vec<LettersCollection>>()
        .into_par_iter()
        .flat_map(|group| cross(group, context))
        .collect::<LettersCollection>()
        .into_iter()
        .unique()
        .sorted_by(score_cmp)
        .into_iter()
        .take(context.population_size)
        .collect();

    if children.len() == 0 {
        return Err(());
    }

    Ok(children)
}

fn score_cmp(a: &LettersPointer, b: &LettersPointer) -> Ordering {
    let a_total = get_score(a.left_score, a.right_score);
    let b_total = get_score(b.left_score, b.right_score);

    b_total.partial_cmp(&a_total).unwrap()
}

fn cross(collection: LettersCollection, context: &Context) -> LettersCollection {
    if collection.len() == 1 {
        return collection;
    }

    let mut crossed = collection
        .iter()
        .tuple_windows()
        .map(|(a, b)| a.cross(&b.mutations, context))
        .collect_vec();

    crossed.extend(collection);

    crossed
}

// an implementation with a thread pool
// let (sender, receiver) = channel();
// pool.scoped(|scoped| {
//     mutants
//         .into_iter()
//         .unique()
//         .sorted_by(score_cmp)
//         .group_by(|x| x.parent_version.clone())
//         .into_iter()
//         .for_each(|(_, group)| {
//             let copy: LettersCollection = group.collect();
//             let sender = sender.clone();
//             scoped.execute(move || {
//                 let result = cross(copy, digraphs);
//                 sender.send(result).unwrap();
//             });
//         });
// });
// let mut set = HashSet::new();
// while let Ok(item) = receiver.try_recv() {
//     set.extend(item);
// }
// let children: LettersCollection = set
//     .into_iter()
//     .sorted_by(score_cmp)
//     .into_iter()
//     .take(context.population_size)
//     .collect();
