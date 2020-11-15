pub mod models;

use models::{Cli, Digraphs, Group};
use std::collections::HashMap;
use std::error::Error;

pub fn run(args: &Cli) -> Result<(), Box<dyn Error>> {
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();

    let digraphs_map = Digraphs::new(digraphs);

    // build left group
    let left_letters: Vec<char> = ('a'..'z').collect();
    let left_group = Group {
        score: digraphs_map.get_score(&left_letters),
        letters: &left_letters,
    };

    // right group is empty
    let right_letters: Vec<char> = Vec::new();
    let right_group = Group {
        score: digraphs_map.get_score(&right_letters),
        letters: &right_letters,
    };

    // 1) get biggest/smallest pair and place it to the right group
    // let biggest_pair = get_biggest_pair(&digraphs_map);
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

    Ok(())
}

fn get_biggest_pair(digraphs_map: &HashMap<char, HashMap<char, f64>>) -> (char, char, f64) {
    todo!()
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
