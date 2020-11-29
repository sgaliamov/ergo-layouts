use ed_balance::models::{DynError, Settings};
use rand::{prelude::SliceRandom, thread_rng, RngCore};
use std::process;
use structopt::StructOpt;

fn main() {
    let args = Settings::from_args();
    if let Err(e) = run(&args) {
        eprintln!("Calculations failed: {:#?}", e);
        process::exit(1);
    }
}

// get a list of instances.
// do mutations. keep mutations as objects.
// calculate scores.
// get the bests mutations.
// cross best mutations.
// apply child mutations.

pub fn run(settings: &Settings) -> Result<(), DynError> {
    todo!()
}

struct Mutation {
    left: char,
    right: char,
}

struct Letters {
    left: Vec<char>,
    right: Vec<char>,
    mutations: Vec<Mutation>,
}

const LEFT_COUNT: usize = 15;
const RIGHT_COUNT: usize = 26 - LEFT_COUNT;

impl Letters {
    pub fn new(mutations_count: Option<usize>) -> Self {
        let mut rng = thread_rng();
        let mut all: Vec<char> = ('a'..='z').collect();
        let mutations_count = match mutations_count {
            Some(value) => value,
            _ => 4,
        };

        all.shuffle(&mut rng);

        Letters {
            left: all.iter().take(LEFT_COUNT).map(|x| *x).collect(),
            right: all.iter().skip(LEFT_COUNT).map(|x| *x).collect(),
            mutations: Vec::with_capacity(mutations_count),
        }
    }

    pub fn mutate(&self, mutations_count: usize) -> Letters {
        let mut rng = thread_rng();
        let mut mutant = Letters {
            left: self.left.clone(),
            right: self.right.clone(),
            mutations: Vec::with_capacity(mutations_count),
        };

        for _ in 0..mutations_count {
            // todo: exclude duplicates
            let left_index = (rng.next_u32() % (LEFT_COUNT as u32)) as usize;
            let right_index = (rng.next_u32() % (RIGHT_COUNT as u32)) as usize;
            let left = mutant.left[left_index];
            let right = mutant.left[right_index];
            mutant.left[left_index] = right;
            mutant.left[right_index] = left;

            mutant.mutations.push(Mutation { left, right });
        }

        mutant
    }
}
