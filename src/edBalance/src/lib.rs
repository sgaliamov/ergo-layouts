pub mod models;

use models::{Cli, Digraphs, Group};
use std::collections::LinkedList;
use std::error::Error;

pub fn run(args: &Cli) -> Result<(), Box<dyn Error>> {
    let digraphs = load_digraph(args)?;

    // build left group
    let left_letters: Vec<char> = ('a'..'z').collect();
    let left_group = Group {
        score: digraphs.calculate_score(&left_letters),
        letters: &left_letters,
    };

    // right group is empty
    let right_letters: Vec<char> = Vec::new();
    let right_group = Group {
        score: digraphs.calculate_score(&right_letters),
        letters: &right_letters,
    };

    // 1) get biggest/smallest pair and place it to the right group
    // 2) find a pair to move to the right group that will give biggest result
    // iterate letters to find which will give biggest sum of both groups
    // calculate value of the left group without current letter
    // calculate right groups with new letter
    // when move joined letters move them all (zxcv, th?, er?)
    // select maximum combination
    // print maximized groups
    // continue till left group has 11 letters (because rest 4 can be used for punctuation keys)

    println!("{:#?}", left_group);
    println!("{:#?}", right_group);
    println!("{:#?}", digraphs);

    Ok(())
}

fn load_digraph(args: &Cli) -> Result<Digraphs, Box<dyn Error>> {
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();

    Ok(Digraphs::new(digraphs))
}

fn get_biggest_pair(
    digraphs: &Digraphs,
    left: &mut LinkedList<char>,
    right: &mut LinkedList<char>,
) {
    let x = '_';
    let y = '_';

    loop {
        let a = left.pop_front().unwrap();
        let left_vec = left.iter().map(|x| *x).collect();
        let left_score = digraphs.calculate_score(&left_vec);

        right.push_back(a);
        let right_vec = right.iter().map(|x| *x).collect();
        let rigth_score = digraphs.calculate_score(&right_vec);
    }
}
