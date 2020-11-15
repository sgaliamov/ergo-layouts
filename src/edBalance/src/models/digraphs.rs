use serde_json::{Map, Value};
use std::collections::HashMap;

#[derive(Debug)]
pub struct Digraphs {
    map: HashMap<char, HashMap<char, f64>>,
}

impl Digraphs {
    // todo: return Result
    pub fn new(json: &Map<String, Value>) -> Digraphs {
        Digraphs {
            map: json
                .iter()
                .map(|(digraph, value)| {
                    // todo: use `?`
                    let first = match digraph.chars().nth(0) {
                        Some(v) => v,
                        None => return Err(()),
                    };
                    let second = match digraph.chars().nth(1) {
                        Some(v) => v,
                        None => return Err(()),
                    };
                    let third = match value.as_f64() {
                        Some(v) => v,
                        None => return Err(()),
                    };
                    Ok((first, second, third))
                })
                .fold(HashMap::new(), |mut result, r| match r {
                    Ok((first, second, value)) => {
                        result
                            .entry(first)
                            .or_insert(HashMap::new())
                            .insert(second, value);
                        result
                            .entry(second)
                            .or_insert(HashMap::new())
                            .insert(first, value);
                        result
                    }
                    Err(_) => result,
                }),
        }
    }

    pub fn get_score(&self, letters: &Vec<char>) -> f64 {
        let mut score = 0.0;
        let mut j = 0;

        while j < letters.len() {
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

    pub fn get_value(&self, first: &char, second: &char) -> f64 {
        match &self.map.get(first).and_then(|inner| inner.get(second)) {
            Some(value) => **value,
            None => 0.0,
        }
    }
}
