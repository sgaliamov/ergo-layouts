use serde_json::{Map, Value};
use std::collections::HashMap;
use std::error::Error;
use std::path::PathBuf;

#[derive(Debug)]
pub struct Digraphs {
    map: DigraphsMap,
}

pub type DigraphsMap = HashMap<char, HashMap<char, f64>>;

impl Digraphs {
    pub fn new(json: &Map<String, Value>) -> Digraphs {
        let map = json
            .iter()
            .map(|(digraph, value)| {
                let first = digraph.chars().nth(0)?;
                let second = digraph.chars().nth(1)?;
                let third = value.as_f64()?;
                Some((first, second, third))
            })
            .map(|some| some.unwrap())
            .fold(DigraphsMap::new(), |mut result, (first, second, value)| {
                result
                    .entry(first)
                    .or_insert(HashMap::new())
                    .insert(second, value);
                result
            });

        Digraphs { map }
    }

    pub fn calculate_score(&self, letters: &Vec<char>) -> f64 {
        if letters.len() == 0 {
            return 0.;
        }

        let mut score = 0.0;
        let mut j = 0;

        while j < letters.len() - 1 {
            let mut i = j + 1;
            let first = letters[j];

            while i < letters.len() {
                let second = letters[i];
                score += self.get_value(&first, &second);
                score += self.get_value(&second, &first);
                i += 1;
            }
            j += 1;
        }

        score
    }

    pub fn load(path: &PathBuf) -> Result<Digraphs, Box<dyn Error>> {
        let content = std::fs::read_to_string(path)?;
        let json: serde_json::Value = serde_json::from_str(&content)?;
        let digraphs = json.as_object().unwrap();

        Ok(Digraphs::new(digraphs))
    }

    fn get_value(&self, first: &char, second: &char) -> f64 {
        match &self.map.get(first).and_then(|inner| inner.get(second)) {
            Some(value) => **value,
            None => 0.0,
        }
    }
}

#[cfg(test)]
pub mod tests {
    use super::*;
    use serde_json::json;

    #[test]
    pub fn calculate_score() {
        let json = json!({
            "ab": 1.0, // straight
            "ba": 2.0, // reverted
            "ef": 3.0, // one extra letter
            "cf": 4.0, // only straight
            "dc": 5.0, // ony reverted
            "xz": 6.0, // not used
        });
        let target = Digraphs::new(&json.as_object().unwrap());
        let letters = vec!['b', 'c', 'd', 'f', 'g', 'a'];
        let actual = target.calculate_score(&letters);

        assert_eq!(
            actual,
            1. + 2. + 4. + 5.,
            "'ef', 'xz' and 'g' are not counted"
        );
    }

    #[test]
    pub fn calculate_score_on_empty_vector() {
        let json = json!({
            "ab": 1.0,
            "bc": 2.0,
        });
        let target = Digraphs::new(&json.as_object().unwrap());
        let letters = Vec::with_capacity(0);
        let actual = target.calculate_score(&letters);

        assert_eq!(actual, 0.);
    }

    #[test]
    pub fn test_child_iterator() {
        let a = vec![1, 2, 3];
        let mut iter = a.iter();
        iter.next();

        let mut child_iter = iter.clone().into_iter();

        assert_eq!(*child_iter.next().unwrap(), 2);
        assert_eq!(*child_iter.next().unwrap(), 3);
        assert_eq!(*iter.next().unwrap(), 2);
    }
}
