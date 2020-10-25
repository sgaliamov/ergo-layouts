use ed_balance::lib::{Cli, Config};
use json5;
use structopt::StructOpt;

fn main() {
    let args = Cli::from_args();
    let content = std::fs::read_to_string(&args.digraphs).expect("could not read file");
    let data: Config = json5::from_str(&content).expect("wrong json");
    // build left group
    // right group is empty
    // 1) get biggest/smallest pair and place it to the right group
    // 2) find a pair to move to the right group that will give biggest result
    // iterate letters to find which will give biggest sum of both groups
    // calculate value of the left group without current letter
    // calculate right groups with new letter
    // when move joined letters move them all (zxcv, th?, er?)
    // select maximum combination
    // print maximized groups
    // continue till left group has 11 letters (because rest 4 can be used for punctuation keys)

    print!("{:#?}", data);
}
