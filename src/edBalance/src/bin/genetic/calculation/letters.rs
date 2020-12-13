use ed_balance::models::Digraphs;
use itertools::Itertools;
use rand::{distributions::Alphanumeric, prelude::SliceRandom, thread_rng, Rng};
use std::hash::Hash;

#[derive(Debug, Hash, Eq, PartialEq, PartialOrd, Clone, Copy)]
pub struct Mutation {
    pub left: char,
    pub right: char,
}

#[derive(Debug, Clone)]
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

impl Eq for Letters {}

impl PartialEq for Letters {
    fn eq(&self, other: &Letters) -> bool {
        self.left.eq(&other.left) && self.right.eq(&other.right)
    }
}

impl Hash for Letters {
    fn hash<H>(&self, state: &mut H)
    where
        H: std::hash::Hasher,
    {
        self.left.hash(state);
        self.right.hash(state);
    }
}

impl Letters {
    fn ctor(
        version: String,
        left: &Vec<char>,
        right: &Vec<char>,
        mutations: Vec<Mutation>,
        parent_version: String,
        parent_left: Vec<char>,
        parent_right: Vec<char>,
        digraphs: &Digraphs,
    ) -> LettersPointer {
        let mut sorted_left = left.clone();
        let mut sorted_right = right.clone();
        sorted_left.sort();
        sorted_right.sort();

        let left_score = digraphs.calculate_score(&sorted_left);
        let right_score = digraphs.calculate_score(&sorted_right);

        box_letters(Letters {
            left: sorted_left,
            right: sorted_right,
            left_score,
            right_score,
            version,
            mutations,
            parent_version,
            parent_left,
            parent_right,
        })
    }

    pub fn new(digraphs: &Digraphs, left_count: usize) -> LettersPointer {
        let mut all: Vec<_> = ('a'..='z').collect();
        all.shuffle(&mut rand::thread_rng());

        let left = all.iter().take(left_count).map(|&x| x).collect();
        let right = all.iter().skip(left_count).map(|&x| x).collect();
        let version = get_version();

        Self::ctor(
            version.clone(),
            &left,
            &right,
            Vec::new(),
            version, // versions match to be able cross children with parents
            left.clone(),
            right.clone(),
            digraphs,
        )
    }

    pub fn cross(
        &self,
        partner_mutations: &Vec<Mutation>,
        mutations_count: usize,
        digraphs: &Digraphs,
    ) -> LettersPointer {
        let mut left = self.parent_left.clone();
        let mut right = self.parent_right.clone();
        let mut mutations: Vec<_> = self
            .mutations
            .iter()
            .chain(partner_mutations.iter())
            .unique()
            .map(|&x| x)
            .collect();

        mutations.shuffle(&mut rand::thread_rng());

        for mutation in mutations.iter().take(mutations_count) {
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

        Self::ctor(
            get_version(),
            &left,
            &right,
            mutations, // this mutations is not just a sum of 2 mutations, it's an intersection.
            self.parent_version.clone(), // so, to be able to get the current state,
            self.parent_left.clone(), // we have apply this mutations on the initial parent letters.
            self.parent_right.clone(), // current - mutations = parent.
            digraphs,
        )
    }

    pub fn mutate(&self, mutations_count: usize, digraphs: &Digraphs) -> LettersPointer {
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

        Self::ctor(
            get_version(),
            &left,
            &right,
            mutations,
            self.version.clone(),
            self.left.clone(),
            self.right.clone(),
            &digraphs,
        )
    }
}

fn get_version() -> String {
    rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(10)
        .collect::<String>()
}

fn box_letters(letters: Letters) -> LettersPointer {
    Box::new(letters)
}

#[cfg(test)]
pub mod tests {
    use super::*;
    use serde_json::json;

    #[test]
    fn unique_should_work() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let a = Letters::new(&digraphs, 15);
        let b = Letters::new(&digraphs, 15);
        let clone = a.clone();
        let vec: LettersCollection = vec![a, b, clone];

        let actual: LettersCollection = vec.into_iter().unique().collect();

        assert_eq!(actual.len(), 2);
    }

    #[test]
    fn should_assign_parent_version() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());

        let target = Letters::new(&digraphs, 15);
        let actual = target.mutate(1, &digraphs);

        assert_eq!(actual.parent_version, target.version);
    }

    #[test]
    fn should_not_mutate_source_object() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let target = Letters::new(&digraphs, 15);
        let copy = target.left.clone();

        let actual = target.mutate(10, &digraphs);

        assert_ne!(actual.left, copy);
        assert_eq!(copy, target.left);
    }

    #[test]
    fn should_mutate() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let target = Letters::new(&digraphs, 15);

        let actual = target.mutate(10, &digraphs);

        assert_ne!(target.left, actual.left);
        assert_ne!(target.right, actual.right);
    }

    #[test]
    fn should_sort_chars() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let letters = Letters::new(&digraphs, 15);

        let target = to_sorted_string(&letters.left);
        let actual: String = letters.left.iter().collect();

        assert_eq!(target, actual);

        let target = to_sorted_string(&letters.right);
        let actual: String = letters.right.iter().collect();

        assert_eq!(target, actual);
    }

    fn to_sorted_string(list: &Vec<char>) -> String {
        let mut vec = list.clone();
        vec.sort();
        vec.iter().collect()
    }
}

pub type LettersPointer = Box<Letters>;

pub type LettersCollection = Vec<LettersPointer>;
