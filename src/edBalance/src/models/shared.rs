use std::error::Error;
use std::path::PathBuf;
use structopt::StructOpt;

#[derive(StructOpt)]
pub struct Settings {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,

    #[structopt(short = "f", long = "frozen", default_value = ".")]
    pub frozen: String,

    #[structopt(short = "m", long = "mutations-count", default_value = "2")]
    pub mutations_count: usize,

    #[structopt(short = "s", long = "population-size", default_value = "100")]
    pub population_size: usize,

    #[structopt(short = "c", long = "children-count", default_value = "100")]
    pub children_count: u16,

    #[structopt(short = "m", long = "generations-count", default_value = "1000")]
    pub generations_count: usize,
}

pub type DynError = Box<dyn Error>;
