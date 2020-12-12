mod letters;
mod process;

use ed_balance::models::{print_letters, Digraphs, DynError, Settings};
use indicatif::{ProgressBar, ProgressStyle};
use letters::Letters;
use scoped_threadpool::Pool;

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

pub fn run(settings: &Settings) -> Result<(), DynError> {
    // todo: print progress
    let pb = ProgressBar::new(settings.generations_count as u64);
    pb.set_style(
        ProgressStyle::default_bar()
            .template("[{elapsed_precise}] [{bar:40.cyan/blue}] {bytes}/{total_bytes} ({eta})"),
    );

    let digraphs = Digraphs::load(&settings.digraphs)?;
    let mut pool = Pool::new(num_cpus::get() as u32);
    let mut population: Vec<_> = (0..settings.population_size)
        .into_iter()
        .map(|_| Letters::new(&digraphs))
        .collect();

    for _ in 0..settings.generations_count {
        population = process::run(&mut pool, &mut population, &digraphs, &settings);
        if population.len() == 0 {
            panic!("All died!");
        }
        pb.inc(1);
    }

    for item in population.iter().take(10) {
        print_letters(&item.left, &item.right, item.left_score, item.right_score);
    }

    Ok(())
}
