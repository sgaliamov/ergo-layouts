use ed_balance::models::Digraphs;
use itertools::Itertools;
use rand::{distributions::Alphanumeric, prelude::SliceRandom, thread_rng, Rng};
use std::rc::Rc;

#[derive(Debug, Hash, Eq, PartialEq, Clone, Copy)]
pub struct Mutation {
    pub left: char,
    pub right: char,
}

#[derive(Debug)]
pub struct Letters {
    pub version: String,
    pub left: Vec<char>,
    pub right: Vec<char>,
    pub left_score: f64,
    pub right_score: f64,
    pub mutations: Vec<Mutation>,
    pub parent_version: String,
    pub parent_left: Vec<char>,
    pub parent_right: Vec<char>,
}

impl Letters {
    pub fn new(digraphs: &Digraphs) -> Rc<Self> {
        let mut all: Vec<_> = ('a'..='z').collect();
        all.shuffle(&mut rand::thread_rng());

        let left = all.iter().take(LEFT_COUNT).map(|&x| x).collect();
        let right = all.iter().skip(LEFT_COUNT).map(|&x| x).collect();
        let left_score = digraphs.calculate_score(&left);
        let right_score = digraphs.calculate_score(&right);
        let version = get_version();

        Rc::new(Letters {
            left: left.clone(),
            right: right.clone(),
            left_score,
            right_score,
            version: version.clone(),
            mutations: Vec::new(),
            parent_version: version, // versions match to be able cross children with parents
            parent_left: left,
            parent_right: right,
        })
    }

    pub fn cross(&self, partner_mutations: &Vec<Mutation>, digraphs: &Digraphs) -> LettersSP {
        let mut left = self.parent_left.clone();
        let mut right = self.parent_right.clone();
        let mutations: Vec<_> = self
            .mutations
            .iter()
            .chain(partner_mutations.iter())
            .unique()
            .map(|&x| x)
            .collect();

        for mutation in mutations.iter() {
            let left_index = left.iter().position(|&x| x == mutation.left);
            let right_index = right.iter().position(|&x| x == mutation.right);

            match (left_index, right_index) {
                (Some(left_index), Some(right_index)) => {
                    left[left_index] = mutation.left;
                    right[right_index] = mutation.right;
                }
                _ => panic!("Incompatible mutation!"),
            }
        }

        Rc::new(Letters {
            version: get_version(),
            left_score: digraphs.calculate_score(&left),
            right_score: digraphs.calculate_score(&right),
            left,
            right,
            mutations: mutations.clone(), // this mutations is not just a sum of 2 mutations, it's an intersection.
            parent_version: self.parent_version.clone(), // so, to be able to get the current state,
            parent_left: self.parent_left.clone(), // we have apply this mutations on the initial parent letters.
            parent_right: self.parent_right.clone(), // current - mutations = parent.
        })
    }

    pub fn mutate(&self, mutations_count: usize, digraphs: &Digraphs) -> LettersSP {
        let mut rng = thread_rng();
        let mut left = self.left.clone();
        let mut right = self.right.clone();
        let mut mutations: Vec<_> = Vec::with_capacity(mutations_count);
        left.shuffle(&mut rng);
        right.shuffle(&mut rng);

        for index in 0..mutations_count {
            let left_char = left[index];
            let right_char = right[index];

            left[index] = right_char;
            right[index] = left_char;

            mutations.push(Mutation {
                left: left_char,
                right: right_char,
            });
        }

        Rc::new(Letters {
            left_score: digraphs.calculate_score(&left),
            right_score: digraphs.calculate_score(&right),
            left,
            right,
            mutations,
            version: get_version(),
            parent_version: self.version.clone(),
            parent_left: self.left.clone(),
            parent_right: self.right.clone(),
        })
    }
}

const LEFT_COUNT: usize = 15;

fn get_version() -> String {
    rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(10)
        .collect::<String>()
}

#[cfg(test)]
pub mod tests {
    use super::*;
    use serde_json::json;

    #[test]
    pub fn should_assign_parent_version() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());

        let target = Letters::new(&digraphs);
        let actual = target.mutate(1, &digraphs);

        assert_eq!(actual.parent_version, target.version);
    }

    #[test]
    pub fn should_not_mutate_source_object() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let target = Letters::new(&digraphs);
        let copy = target.left.clone();

        let actual = target.mutate(10, &digraphs);

        assert_ne!(actual.left, copy);
        assert_eq!(copy, target.left);
    }

    #[test]
    pub fn should_mutate() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let target = Letters::new(&digraphs);

        let actual = target.mutate(10, &digraphs);

        assert_ne!(target.left, actual.left);
        assert_ne!(target.right, actual.right);
    }
}

pub type LettersSP = Rc<Letters>;

pub type LettersCollection = Vec<LettersSP>;
