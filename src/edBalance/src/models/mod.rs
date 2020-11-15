mod digraphs;
mod group;

use std::path::PathBuf;
use structopt::StructOpt;

pub use digraphs::*;
pub use group::*;

#[derive(StructOpt)]
pub struct Cli {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,
}
