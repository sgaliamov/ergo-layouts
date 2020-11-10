mod cli;
mod lib;

use crate::cli::Cli;
use crate::lib::custom::create_digraphs_map;
use std::error::Error;
use structopt::StructOpt;

#[derive(Debug)]
struct Group {
    letters: Vec<char>,
    score: i32,
}

#[cfg(test)]
pub mod tests {
    #[test]
    pub fn my_test_main() {
        assert_eq!(2 + 2, 43);
    }
}

fn main() -> Result<(), Box<dyn Error>> {
    let args = Cli::from_args();
    let content = std::fs::read_to_string(&args.digraphs)?;
    let json: serde_json::Value = serde_json::from_str(&content)?;
    let digraphs = json.as_object().unwrap();
    let digraphs_map = create_digraphs_map(digraphs);

    // build left group
    let left_group = Group {
        score: 0,
        letters: ('a'..'z').collect(),
    };

    // right group is empty
    let right_group = Group {
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

    Ok(())
}
