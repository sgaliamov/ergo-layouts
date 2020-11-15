pub mod models;

use models::{Cli, Digraphs};
use std::collections::LinkedList;
use std::error::Error;

pub fn run(args: &Cli) -> Result<(), Box<dyn Error>> {
    let digraphs = load_digraph(args)?;

    let mut left_letters: LinkedList<char> =
        ('a'..'z').filter(|x| *x != 't' && *x != 'h').collect();

    let mut right_letters: LinkedList<char> = LinkedList::new();
    right_letters.extend(['t', 'h'].iter());

    // 1) get biggest/smallest pair and place it to the right group
    // 2) find a pair to move to the right group that will give biggest result
    // iterate letters to find which will give biggest sum of both groups
    // calculate value of the left group without current letter
    // calculate right groups with new letter
    // when move joined letters move them all (zxcv, th?, er?)
    // select maximum combination
    // print maximized groups
    // continue till left group has 11 letters (because rest 4 can be used for punctuation keys)
    loop {
        let letter = get_letter_to_move(&digraphs, &mut left_letters, &mut right_letters);
        left_letters.pop_back();
        right_letters.push_back(letter);

        if left_letters.len() <= 15 {
            println!("{:#?}", left_letters);
            println!("{:#?}", right_letters);
        }
        if left_letters.len() <= 11 {
            break;
        }
    }

    Ok(())
}

fn load_digraph(args: &Cli) -> Result<Digraphs, Box<dyn Error>> {
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();

    Ok(Digraphs::new(digraphs))
}

fn get_letter_to_move(
    digraphs: &Digraphs,
    left_letters: &mut LinkedList<char>,
    right_letters: &mut LinkedList<char>,
) -> char {
    let mut max_left = 0.;
    let mut max_right = 0.;
    let mut result = None;
    let mut i = 0;

    while i < left_letters.len() {
        if let Some(letter) = left_letters.pop_front() {
            let left_vec = left_letters.iter().map(|x| *x).collect();
            let left_score = digraphs.calculate_score(&left_vec);

            right_letters.push_back(letter);
            let right_vec = right_letters.iter().map(|x| *x).collect();
            let right_score = digraphs.calculate_score(&right_vec);

            let total_score = left_score + right_score;
            if total_score > max_left + max_right {
                max_left = left_score;
                max_right = right_score;
                result = Some(letter);
            }

            left_letters.push_back(letter);
            right_letters.pop_back();
        } else {
            break;
        }
        i += 1;
    }

    result.unwrap()
}
