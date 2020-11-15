mod digraphs;

use std::path::PathBuf;
use structopt::StructOpt;

pub use digraphs::*;

#[derive(StructOpt)]
pub struct Cli {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,

    #[structopt(short = "f", long = "frozen", default_value = ".")]
    pub frozen: String,
}
