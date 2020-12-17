use std::{cmp::Ordering, collections::HashSet, error::Error, path::PathBuf};
use structopt::StructOpt;

use super::Digraphs;

#[derive(StructOpt)]
pub struct CliSettings {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,

    #[structopt(long = "frozen-left", default_value = "")]
    pub frozen_left: String,

    #[structopt(long = "frozen-right", default_value = "")]
    pub frozen_right: String,

    #[structopt(short = "m", long = "mutations-count", default_value = "2")]
    pub mutations_count: u8,

    #[structopt(short = "p", long = "population-size", default_value = "10")]
    pub population_size: u16,

    #[structopt(short = "c", long = "children-count", default_value = "10")]
    pub children_count: u16,

    #[structopt(short = "g", long = "generations-count", default_value = "100")]
    pub generations_count: u16,

    #[structopt(short = "r", long = "results-count", default_value = "20")]
    pub results_count: u8,

    #[structopt(short = "l", long = "left-count", default_value = "15")]
    pub left_count: u8,

    #[structopt(long = "repeats-count", default_value = "100")]
    pub repeats_count: u16,
}

pub struct Context {
    pub digraphs: Digraphs,
    pub frozen_left: HashSet<char>,
    pub frozen_right: HashSet<char>,
    pub mutations_count: usize,
    pub population_size: usize,
    pub children_count: u16,
    pub generations_count: u16,
    pub results_count: usize,
    pub left_count: usize,
    pub repeats_count: u16,
}

impl Context {
    pub fn new(settings: &CliSettings) -> Self {
        let digraphs = Digraphs::load(&settings.digraphs).unwrap();

        let mut frozen_left = HashSet::with_capacity(settings.frozen_left.len());
        frozen_left.extend(settings.frozen_left.chars());

        let mut frozen_right = HashSet::with_capacity(settings.frozen_right.len());
        frozen_right.extend(settings.frozen_right.chars());

        Context {
            digraphs,
            frozen_left,
            frozen_right,
            mutations_count: settings.mutations_count as usize,
            population_size: settings.population_size as usize,
            children_count: settings.children_count,
            generations_count: settings.generations_count,
            results_count: settings.results_count as usize,
            left_count: settings.left_count as usize,
            repeats_count: settings.repeats_count,
        }
    }

    pub fn default(digraphs: Digraphs) -> Self {
        Context {
            digraphs,
            frozen_left: HashSet::new(),
            frozen_right: HashSet::new(),
            mutations_count: 4,
            population_size: 10,
            children_count: 10,
            generations_count: 10,
            results_count: 10,
            left_count: 15,
            repeats_count: 10,
        }
    }
}

pub type DynError = Box<dyn Error>;

fn get_factor(left_score: f64, right_score: f64) -> f64 {
    let factor = if left_score.partial_cmp(&right_score).unwrap() == Ordering::Less {
        left_score / right_score
    } else {
        right_score / left_score
    };

    1.1 - 0.1 / factor
}

pub fn get_score(left: f64, right: f64) -> f64 {
    let factor = get_factor(left, right);
    let total = left + right;

    total * factor * factor * factor
}

pub fn format_result(
    left_letters: &Vec<char>,
    right_letters: &Vec<char>,
    left_score: f64,
    right_score: f64,
) -> String {
    let left_string: String = left_letters.iter().collect();
    let right_string: String = right_letters.iter().collect();

    format!(
        "{}; {}; {:.3}; {}; {}; {:.3}; {:.3}; {:.3}; {:.3};",
        left_letters.len(),
        left_string,
        left_score,
        right_letters.len(),
        right_string,
        right_score,
        get_factor(left_score, right_score),
        left_score + right_score,
        get_score(left_score, right_score)
    )
}

pub fn print_letters(
    left_letters: &Vec<char>,
    right_letters: &Vec<char>,
    left_score: f64,
    right_score: f64,
) {
    println!(
        "{}",
        format_result(left_letters, right_letters, left_score, right_score)
    );
}
