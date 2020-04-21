export interface RatingQuestion {
  ratingQuestionId: string;
  question: string;
}

export function isRatingQuestion(x: any): x is RatingQuestion {
  return typeof x.ratingQuestionId === 'string' && typeof x.question === 'string';
}
export function isRatingQuestionArray(x: any): x is RatingQuestion[] {
  return x.every(isRatingQuestion);
}
