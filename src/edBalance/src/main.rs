mod cli;

use cli::Cli;
use serde_json::{json, Map, Value};
use std::collections::HashMap;
use structopt::StructOpt;

#[derive(Debug)]
struct Group {
    letters: Vec<char>,
    score: i32,
}

fn create_digraphs_map(json: &Map<String, Value>) -> HashMap<char, HashMap<char, f64>> {
    let mut dig_map: HashMap<char, HashMap<char, f64>> = HashMap::new();

    for (digraph, value) in json {
        let first = digraph.chars().nth(0).unwrap();
        let second = digraph.chars().nth(1).unwrap();
        let inner_map = dig_map.entry(first).or_insert(HashMap::new());
        inner_map.insert(second, value.as_f64().unwrap());
    }

    return dig_map;
}

fn main() {
    let args = Cli::from_args();
    let content = std::fs::read_to_string(&args.digraphs).expect("could not read file");
    let json = json!(content);
    let digraphs = json.as_object().unwrap();
    let digraphs_map = create_digraphs_map(digraphs);

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
    println!("{:#?}", digraphs_map);
}
