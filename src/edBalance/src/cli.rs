use std::path::PathBuf;
use structopt::StructOpt;

#[derive(StructOpt)]
pub struct Cli {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,
}
