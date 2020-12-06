use ed_balance::models::Digraphs;
use rand::{distributions::Alphanumeric, prelude::SliceRandom, thread_rng, Rng};

#[derive(Debug, Eq, PartialEq, Hash, Clone)]
pub struct Mutation {
    pub left: char,
    pub right: char,
}

#[derive(Debug)]
pub struct Letters {
    pub left: Vec<char>,
    pub right: Vec<char>,
    pub left_score: f64,
    pub right_score: f64,
    pub version: Box<String>,
    pub parent_version: Box<String>,
    pub mutations: Vec<Mutation>,
}

impl Letters {
    pub fn new(digraphs: &Digraphs) -> Box<Self> {
        let mut all: Vec<_> = ('a'..='z').collect();
        all.shuffle(&mut rand::thread_rng());

        let left: Vec<_> = all.iter().take(LEFT_COUNT).map(|x| *x).collect();
        let right: Vec<_> = all.iter().skip(LEFT_COUNT).map(|x| *x).collect();
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

fn get_version() -> Box<String> {
    Box::new(
        rand::thread_rng()
            .sample_iter(&Alphanumeric)
            .take(10)
            .collect::<String>(),
    )
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

        target.mutate(10, &digraphs);

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
