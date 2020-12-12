mod letters;
mod process;

use ed_balance::models::{print_letters, Digraphs, DynError, Settings};
use letters::Letters;
use scoped_threadpool::Pool;

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

    (0..settings.generations_count).for_each(|_| {
        population = process::run(&mut pool, &mut population, &digraphs, &settings);
        if population.len() == 0 {
            panic!("All died!");
        }
    });

    for item in population.iter().take(10) {
        print_letters(&item.left, &item.right, item.left_score, item.right_score);
    }

    Ok(())
}
