mod cli;

use cli::Cli;
use serde_json::json;
use structopt::StructOpt;

#[derive(Debug)]
struct Group {
    letters: Vec<char>,
    score: i32,
}

fn main() {
    let args = Cli::from_args();
    let content = std::fs::read_to_string(&args.digraphs).expect("could not read file");
    let digraphs = json!(content);

    // build left group
    let mut left_group = Group {
        score: 0,
        letters: ('a'..'z').collect(),
    };

    // right group is empty
    let mut right_group = Group {
        score: 0,
        letters: Vec::new(),
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
}
