use ed_balance::models::{Context, Digraphs};
use itertools::{min, Itertools};
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

    pub fn copy(&self) -> LettersPointer {
        box_letters(Letters {
            left: self.left.clone(),
            right: self.right.clone(),
            left_score: self.left_score,
            right_score: self.right_score,
            version: self.version.clone(),
            mutations: self.mutations.clone(),
            parent_version: self.parent_version.clone(),
            parent_left: self.parent_left.clone(),
            parent_right: self.parent_right.clone(),
        })
    }

    pub fn new(context: &Context) -> LettersPointer {
        let mut all = ('a'..='z')
            .filter(|&x| !context.frozen_right.contains(&x))
            .filter(|&x| !context.frozen_left.contains(&x))
            .collect_vec();

        all.shuffle(&mut rand::thread_rng());

        let mut left = context.frozen_left.iter().map(|&x| x).collect_vec();
        left.append(
            &mut all
                .iter()
                .take(context.left_count - left.len())
                .map(|&x| x)
                .collect(),
        );

        let mut right = context.frozen_right.iter().map(|&x| x).collect_vec();
        right.append(
            &mut all
                .iter()
                .filter(|x| !left.contains(x))
                .map(|&x| x)
                .collect(),
        );

        let version = get_version();

        Self::ctor(
            version.clone(),
            &left,
            &right,
            Vec::new(),
            version, // versions match to be able cross children with parents
            left.clone(),
            right.clone(),
            &context.digraphs,
        )
    }

    pub fn cross(&self, partner_mutations: &Vec<Mutation>, context: &Context) -> LettersPointer {
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

        for mutation in mutations.iter().take(context.mutations_count) {
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
            &context.digraphs,
        )
    }

    pub fn mutate(&self, context: &Context) -> LettersPointer {
        let mut rng = thread_rng();

        let mut left = self
            .left
            .iter()
            .filter(|&x| !context.frozen_left.contains(x))
            .map(|&x| x)
            .collect_vec();
        left.shuffle(&mut rng);

        let mut right = self
            .right
            .iter()
            .filter(|&x| !context.frozen_right.contains(x))
            .map(|&x| x)
            .collect_vec();
        right.shuffle(&mut rng);

        let mut mutations: Vec<_> = Vec::with_capacity(context.mutations_count);

        let mutations_count = min(vec![context.mutations_count, left.len(), right.len()]).unwrap();

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

        left.extend(&context.frozen_left.iter().map(|&x| x).collect_vec());
        right.extend(&context.frozen_right.iter().map(|&x| x).collect_vec());

        Self::ctor(
            get_version(),
            &left,
            &right,
            mutations,
            self.version.clone(),
            self.left.clone(),
            self.right.clone(),
            &context.digraphs,
        )
    }
}

fn get_version() -> String {
    rand::thread_rng()
        .sample_iter(&Alphanumeric)
        .take(10)
        .collect()
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
        let context = Context::default(digraphs);
        let a = Letters::new(&context);
        let b = Letters::new(&context);
        let clone = a.clone();
        let vec: LettersCollection = vec![a, b, clone];

        let actual: LettersCollection = vec.into_iter().unique().collect();

        assert_eq!(actual.len(), 2);
    }

    #[test]
    fn should_assign_parent_version() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let mut context = Context::default(digraphs);
        context.mutations_count = 1;

        let target = Letters::new(&context);
        let actual = target.mutate(&context);

        assert_eq!(actual.parent_version, target.version);
    }

    #[test]
    fn should_not_mutate_source_object() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let context = Context::default(digraphs);
        let target = Letters::new(&context);
        let copy = target.left.clone();
        let actual = target.mutate(&context);

        assert_ne!(actual.left, copy);
        assert_eq!(copy, target.left);
    }

    #[test]
    fn should_mutate() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let context = Context::default(digraphs);
        let target = Letters::new(&context);

        let actual = target.mutate(&context);

        assert_ne!(target.left, actual.left);
        assert_ne!(target.right, actual.right);
    }

    #[test]
    fn should_sort_chars() {
        let json = json!({});
        let digraphs = Digraphs::new(&json.as_object().unwrap());
        let context = Context::default(digraphs);
        let letters = Letters::new(&context);

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
