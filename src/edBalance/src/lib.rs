pub mod models;
use models::{Cli, Digraphs};
use std::collections::LinkedList;
use std::error::Error;

pub fn run(args: &Cli) -> Result<(), Box<dyn Error>> {
    let digraphs = load_digraph(args)?;
    for letter in 'a'..='z' {
        start_with(&letter, &digraphs, &args.frozen)
    }

    Ok(())
}

fn start_with(letter: &char, digraphs: &Digraphs, frozen: &str) {
    let mut right_letters: LinkedList<char> = LinkedList::new();
    right_letters.push_back(*letter);

    let mut left_letters: LinkedList<char> =
        ('a'..='z').filter(|x| !right_letters.contains(x)).collect();

    loop {
        let (letter, index, left_score, right_score) =
            get_letter_to_move(&digraphs, &mut left_letters, &mut right_letters, &frozen);

        let mut split = left_letters.split_off(index);
        split.pop_front();
        left_letters.append(&mut split);
        right_letters.push_back(letter);

        if left_letters.len() <= 15 {
            let left = to_sorted_string(&left_letters);
            let right = to_sorted_string(&right_letters);
            println!(
                "{}; {:.3}; {:.3}; {}; {:.3}; {}",
                left_letters.len(),
                left_score / right_score,
                left_score,
                left,
                right_score,
                right
            );
        }
        if left_letters.len() <= 11 {
            break;
        }
    }
}

fn to_sorted_string(list: &LinkedList<char>) -> String {
    let mut vec = to_vec(list);
    vec.sort();
    vec.iter().collect()
}

fn load_digraph(args: &Cli) -> Result<Digraphs, Box<dyn Error>> {
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();

    Ok(Digraphs::new(digraphs))
}

fn to_vec(list: &LinkedList<char>) -> Vec<char> {
    list.iter().map(|x| *x).collect()
}

fn get_letter_to_move(
    digraphs: &Digraphs,
    left_letters: &mut LinkedList<char>,
    right_letters: &mut LinkedList<char>,
    frozen: &str,
) -> (char, usize, f64, f64) {
    let mut left_result = 0.;
    let mut right_result = 0.;
    let mut result = None;
    let mut i = 0;
    let mut index = 0;
    let mut min_total = 100000.;

    while i < left_letters.len() {
        if let Some(letter) = left_letters.pop_front() {
            if !frozen.contains(letter) {
                let left_vec = to_vec(left_letters);
                let left_score = digraphs.calculate_score(&left_vec);

                right_letters.push_back(letter);
                let right_vec = to_vec(right_letters);
                let right_score = digraphs.calculate_score(&right_vec);

                let total_score = (1. - left_score / right_score).abs();
                if total_score < min_total {
                    left_result = left_score;
                    right_result = right_score;
                    result = Some(letter);
                    index = i;
                    min_total = total_score;
                }

                right_letters.pop_back();
            }

            left_letters.push_back(letter);
        } else {
            panic!("should not get here");
        }
        i += 1;
    }

    (
        result.expect("failed to find a letter to move"),
        index,
        left_result,
        right_result,
    )
}

// 1) get biggest/smallest pair and place it to the right group
// 2) find a pair to move to the right group that will give biggest result
// iterate letters to find which will give biggest sum of both groups
// calculate value of the left group without current letter
// calculate right groups with new letter
// when move joined letters move them all (zxcv, th?, er?)
// select maximum combination
// print maximized groups
// continue till left group has 11 letters (because rest 4 can be used for punctuation keys)
