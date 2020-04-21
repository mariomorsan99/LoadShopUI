export function formatDate(date: Date): string {
  if (!date) {
    return null;
  }
  return date.toJSON().slice(0, 10);
}
