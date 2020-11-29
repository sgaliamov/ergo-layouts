use ed_balance::models::Digraphs;
use rand::{distributions::Alphanumeric, prelude::SliceRandom, thread_rng, Rng, RngCore};
use std::collections::VecDeque;

#[derive(Debug, Eq, PartialEq, Hash, Clone)]
pub struct Mutation {
    pub left: char,
    pub right: char,
}

#[derive(Debug)]
pub struct Letters {
    pub left: VecDeque<char>,
    pub right: VecDeque<char>,
    pub left_score: f64,
    pub right_score: f64,
    pub version: String, // todo: box?
    pub parent_version: String,
    pub mutations: Vec<Mutation>,
}

impl Letters {
    pub fn new(digraphs: &Digraphs) -> Box<Self> {
        let mut all: Vec<char> = ('a'..='z').collect();
        all.shuffle(&mut rand::thread_rng());

        let left = all.iter().take(LEFT_COUNT).map(|x| *x).collect();
        let right = all.iter().skip(LEFT_COUNT).map(|x| *x).collect();
        let left_score = digraphs.calculate_score(&left);
        let right_score = digraphs.calculate_score(&right);
        let version = get_version();

        Box::new(Letters {
            left,
            right,
            mutations: Vec::new(),
            left_score,
            right_score,
            version: version.clone(), // versions match to be able cross children with parents
            parent_version: version.clone(),
        })
    }

    pub fn apply(&self, mutations: &Vec<&Mutation>, digraphs: &Digraphs) -> Box<Letters> {
        let mut left = self.left.clone();
        let mut right = self.right.clone();

        for mutation in mutations.iter() {
            let left_index = left.iter().position(|x| x == &mutation.left);
            let right_index = right.iter().position(|x| x == &mutation.right);

            match (left_index, right_index) {
                (Some(left_index), Some(right_index)) => {
                    left[left_index] = mutation.right;
                    right[right_index] = mutation.left;
                }
                _ => eprintln!("Incompatible mutation!"),
            }
        }

        Box::new(Letters {
            left_score: digraphs.calculate_score(&left),
            right_score: digraphs.calculate_score(&right),
            left,
            right,
            mutations: Vec::new(),
            version: get_version(),
            parent_version: self.version.clone(),
        })
    }

    pub fn mutate(&self, mutations_count: usize, digraphs: &Digraphs) -> Box<Letters> {
        let mut rng = thread_rng();
        let mut left = self.left.clone();
        let mut right = self.right.clone();
        let mut mutations: Vec<Mutation> = Vec::with_capacity(mutations_count);

        for _ in 0..mutations_count {
            // todo: exclude duplicates?
            let left_index = (rng.next_u32() % (left.len() as u32)) as usize;
            let right_index = (rng.next_u32() % (right.len() as u32)) as usize;

            let left_char = left[left_index];
            let right_char = right[right_index];

            left[left_index] = right_char;
            right[right_index] = left_char;

            mutations.push(Mutation {
                left: left_char,
                right: right_char,
            });
        }

        Box::new(Letters {
            left_score: digraphs.calculate_score(&left),
            right_score: digraphs.calculate_score(&right),
            left,
            right,
            mutations,
            version: get_version(),
            parent_version: self.version.clone(),
        })
    }
}

const LEFT_COUNT: usize = 15;
const RIGHT_COUNT: usize = 26 - LEFT_COUNT;

fn get_version() -> String {
    rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(10)
        .collect::<String>()
}
