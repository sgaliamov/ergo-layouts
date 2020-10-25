pub mod lib {
    use serde::Deserialize;
    use std::path::PathBuf;
    use structopt::StructOpt;

    #[derive(StructOpt)]
    pub struct Cli {
        #[structopt(short = "d", long = "digraphs")]
        pub digraphs: PathBuf,
    }

    #[derive(Deserialize, Debug)]
    pub struct Config {
        message: String,
        n: i32,
    }
}
