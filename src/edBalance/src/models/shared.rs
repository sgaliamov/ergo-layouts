use std::error::Error;
use std::path::PathBuf;
use structopt::StructOpt;


#[derive(StructOpt)]
pub struct Cli {
    #[structopt(short = "d", long = "digraphs")]
    pub digraphs: PathBuf,

    #[structopt(short = "f", long = "frozen", default_value = ".")]
    pub frozen: String,

    #[structopt(short = "m", long = "mutations", default_value = "4")]
    pub mutations: u8,

    #[structopt(short = "s", long = "initial-size", default_value = "100")]
    pub initial_size: u16,
}

pub type DynError = Box<dyn Error>;
