export class Directions {
  constructor(public distance: number, public duration: number) { }

  get miles(): number {
    return this.distance / 1609.0;
  }

  get minutes(): number {
    return Math.floor((this.duration / 60.0) % 60.0);
  }

  get hours(): number {
    return Math.floor(this.duration / 60.0 / 60.0);
  }

  get durationDisplay(): string {
    const hours = this.hours;
    const minutes = this.minutes;
    return `${hours} hour${hours > 1 ? 's' : ''} ${minutes} minute${minutes > 1 ? 's' : ''}`;
  }
}
