use ed_balance::models::Cli;
use ed_balance::run;
use std::process;
use structopt::StructOpt;

fn main() {
    let args = Cli::from_args();
    if let Err(e) = run(&args) {
        eprintln!("Calculations failed: {:#?}", e);
        process::exit(1);
    }
}
