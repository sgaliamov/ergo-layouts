use std::{collections::VecDeque, error::Error, path::PathBuf};
use structopt::StructOpt;

#[derive(StructOpt)]
pub struct Settings {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,

    #[structopt(short = "f", long = "frozen", default_value = ".")]
    pub frozen: String,

    #[structopt(short = "m", long = "mutations-count", default_value = "2")]
    pub mutations_count: usize,

    #[structopt(short = "s", long = "population-size", default_value = "10")]
    pub population_size: usize,

    #[structopt(short = "c", long = "children-count", default_value = "10")]
    pub children_count: u16,

    #[structopt(short = "g", long = "generations-count", default_value = "100")]
    pub generations_count: usize,
}

pub type DynError = Box<dyn Error>;

pub const IMBALANCE_FACTOR: f64 = 100.;

pub fn get_imbalance(left_score: f64, right_score: f64) -> f64 {
    (1. - left_score / right_score).abs() / IMBALANCE_FACTOR
}

pub fn print_letters(
    left_letters: &Vec<char>,
    right_letters: &Vec<char>,
    left_score: f64,
    right_score: f64,
) {
    let left = to_sorted_string(&left_letters);
    let right = to_sorted_string(&right_letters);
    let imbalance = get_imbalance(left_score, right_score);

    println!(
        "{}; {}; {:.3}; {}; {}; {:.3}; {:.3}; {:.3};",
        left_letters.len(),
        left,
        left_score,
        right_letters.len(),
        right,
        right_score,
        imbalance * IMBALANCE_FACTOR,
        (left_score + right_score) / imbalance
    );
}

pub fn to_sorted_string(list: &Vec<char>) -> String {
    let mut vec = list.clone();
    vec.sort();
    vec.iter().collect()
}

pub fn to_vec(list: &VecDeque<char>) -> Vec<char> {
    list.iter().map(|x| *x).collect()
}
