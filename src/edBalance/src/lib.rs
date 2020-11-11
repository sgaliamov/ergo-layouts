pub mod models;

use models::Cli;
use serde_json::{Map, Value};
use std::collections::HashMap;
use std::error::Error;

pub fn run(args: &Cli) -> Result<(), Box<dyn Error>> {
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();
    let digraphs_map = create_digraphs_map(digraphs);

    // build left group
    // let left_group = Group {
    //     score: 0,
    //     letters: ('a'..'z').collect(),
    // };

    // right group is empty
    // let right_group = Group {
    //     score: 0,
    //     letters: Vec::new(),
    // };

    // 1) get biggest/smallest pair and place it to the right group
    // 2) find a pair to move to the right group that will give biggest result
    // iterate letters to find which will give biggest sum of both groups
    // calculate value of the left group without current letter
    // calculate right groups with new letter
    // when move joined letters move them all (zxcv, th?, er?)
    // select maximum combination
    // print maximized groups
    // continue till left group has 11 letters (because rest 4 can be used for punctuation keys)

    // println!("{:#?}", left_group);
    // println!("{:#?}", right_group);
    println!("{:#?}", digraphs_map);

    Ok(())
}

fn create_digraphs_map(json: &Map<String, Value>) -> HashMap<char, HashMap<char, f64>> {
    json.iter()
        .map(|(digraph, value)| {
            let first = digraph.chars().nth(0).unwrap();
            let second = digraph.chars().nth(1).unwrap();
            (first, second, value.as_f64().unwrap())
        })
        .fold(HashMap::new(), |mut result, (first, second, value)| {
            result
                .entry(first)
                .or_insert(HashMap::new())
                .insert(second, value);
            result
                .entry(second)
                .or_insert(HashMap::new())
                .insert(first, value);
            result
        })
}

// #[cfg(test)]
// pub mod tests {
//     use super::*;
//     #[test]
//     pub fn my_test_main() {
//         let c = Qwe { value: 5 };

//         assert_eq!(c.foo().clone(), 5, "message");
//     }
// }
