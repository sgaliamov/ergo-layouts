mod letters;
mod process;

use chrono::prelude::*;
use ed_balance::models::{format_result, CliSettings, Context, DynError};
use indicatif::{MultiProgress, ProgressBar, ProgressStyle};
use letters::{Letters, LettersCollection};
use std::{sync::Arc, thread};

pub fn run(settings: CliSettings) -> Result<(), DynError> {
    let settings = Arc::new(settings);
    let progress = MultiProgress::new();

    let pb_main = ProgressBar::new(settings.generations_count as u64);
    pb_main.set_style(
        ProgressStyle::default_bar()
            .template("[{elapsed_precise}] {bar} {pos}/{len} ({eta}) {msg}"),
    );
    let pb_main = progress.add(pb_main);

    let spinner_style = ProgressStyle::default_spinner().template("{wide_msg}");
    let pb_letters: Vec<ProgressBar> = (0..settings.results_count)
        .map(|_| {
            let pb = ProgressBar::new_spinner();
            pb.set_style(spinner_style.clone());
            progress.add(pb)
        })
        .collect();

    let settings = Arc::clone(&settings);
    let _ = thread::spawn(move || {
        let context = Context::new(&settings);

        let mut population = (0..context.population_size)
            .into_iter()
            .map(|_| Letters::new(&context))
            .collect();

        let mut prev: DateTime<Utc> = Utc::now();
        let mut prev_result = LettersCollection::new();
        let mut repeats_counter = 0;
        for index in 0..context.generations_count {
            population = process::run(&mut population, &context).expect("All died!");

            if let Some(date) =
                render_progress(index, prev, &pb_main, &pb_letters, &population, &context)
            {
                prev = date
            }

            if let Some((repeats, top_results)) =
                need_to_continue(repeats_counter, prev_result, &population, &context)
            {
                prev_result = top_results;
                repeats_counter = repeats;
                pb_main.set_message(&format!("[repeats: {}]", repeats_counter));
            } else {
                pb_main.set_message(&format!("[repeats: {}]", repeats_counter + 1));
                break;
            }
        }

        pb_main.finish();
        pb_letters.iter().for_each(|x| x.finish());
    });

    progress.join().unwrap();

    Ok(())
}

fn need_to_continue(
    mut repeats: u16,
    prev_result: LettersCollection,
    population: &LettersCollection,
    context: &Context,
) -> Option<(u16, LettersCollection)> {
    let top_results: Vec<_> = population
        .iter()
        .take(context.results_count)
        .map(|x| x.copy())
        .collect();

    if prev_result.eq(&top_results) {
        repeats += 1;
    } else {
        repeats = 0;
    }

    if repeats == context.repeats_count {
        return None;
    }

    Some((repeats, top_results))
}

fn render_progress(
    index: u16,
    prev: DateTime<Utc>,
    pb_main: &ProgressBar,
    pb_letters: &Vec<ProgressBar>,
    population: &LettersCollection,
    context: &Context,
) -> Option<DateTime<Utc>> {
    let passed = Utc::now() - prev;

    if passed.num_seconds() >= 5 || index == 0 || index == context.generations_count - 1 {
        for (i, item) in population.iter().take(pb_letters.len()).enumerate() {
            let text = format_result(&item.left, &item.right, item.left_score, item.right_score);
            pb_letters[i].set_message(&text);
        }

        pb_main.set_position(index as u64);

        return Some(Utc::now());
    }

    None
}
