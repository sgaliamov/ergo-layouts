use std::path::PathBuf;
use structopt::StructOpt;

#[derive(StructOpt)]
pub struct Cli {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,
}

#[derive(Debug)]
pub struct Group<'a> {
    pub letters: &'a Vec<char>,
    pub score: i32,
}
