mod letters;
mod process;

use ed_balance::models::{format_result, Digraphs, DynError, Settings};
use indicatif::{MultiProgress, ProgressBar, ProgressStyle};
use letters::Letters;
use std::thread;

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

// todo: frozen left and right letters
// todo: apply half of mutations

pub fn run(settings: &Settings) -> Result<(), DynError> {
    let progress = MultiProgress::new();
    let pb_main = ProgressBar::new(settings.generations_count as u64);
    pb_main.set_style(
        ProgressStyle::default_bar()
            .template("[{elapsed_precise}] [{bar:40.cyan/blue}] {pos}/{len} ({eta})"),
    );
    let pb_main = progress.add(pb_main);
    let spinner_style = ProgressStyle::default_spinner()
        .tick_chars("|/-\\ ")
        .template("{spinner} {wide_msg}");
    let pb_letters: Vec<_> = (0..settings.results_count)
        .map(|_| {
            let pb = ProgressBar::new_spinner();
            pb.set_style(spinner_style.clone());
            progress.add(pb)
        })
        .collect();

    let settings = settings.clone();

    let _ = thread::spawn(move || {
        let digraphs = Digraphs::load(&settings.digraphs).unwrap();

        let mut population: Vec<_> = (0..settings.population_size)
            .into_iter()
            .map(|_| Letters::new(&digraphs, settings.left_count))
            .collect();

        for _ in 0..settings.generations_count {
            population = process::run(&mut population, &digraphs, &settings).expect("All died!");

            pb_main.inc(1);
            for (i, item) in population.iter().take(pb_letters.len()).enumerate() {
                let text =
                    format_result(&item.left, &item.right, item.left_score, item.right_score);
                pb_letters[i].set_message(&text);
            }
        }

        pb_main.finish();
        pb_letters.iter().for_each(|x| x.finish());
    });

    progress.join().unwrap();

    Ok(())
}
