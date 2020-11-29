mod calculation;

use calculation::run;
use ed_balance::models::Settings;
use std::process;
use structopt::StructOpt;

fn main() {
    let args = Settings::from_args();
    if let Err(e) = run(&args) {
        eprintln!("Calculations failed: {:#?}", e);
        process::exit(1);
    }
}
