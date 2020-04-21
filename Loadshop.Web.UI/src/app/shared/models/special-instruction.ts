import { SpecialInstructionData, SpecialInstructionEquipmentData } from '.';

export class SpecialInstruction implements SpecialInstructionData {
  specialInstructionId: number;
  name: string;
  description: string;
  comments: string;
  customerId: string;
  originAddress1: string;
  originCity: string;
  originState: string;
  originPostalCode: string;
  originCountry: string;
  destinationAddress1: string;
  destinationCity: string;
  destinationState: string;
  destinationPostalCode: string;
  destinationCountry: string;
  specialInstructionEquipment: SpecialInstructionEquipmentData[];
  public constructor(init?: Partial<SpecialInstruction>) {
    Object.assign(this, init);
  }

  get originDescription(): string {
    return this.formatLocation(this.originAddress1, this.originCity, this.originState, this.originPostalCode, this.originCountry);
  }

  get destinationDescription(): string {
    return this.formatLocation(
      this.destinationAddress1,
      this.destinationCity,
      this.destinationState,
      this.destinationPostalCode,
      this.destinationCountry
    );
  }

  private formatLocation(address: string, city: string, state: string, postalCode: string, country: string) {
    let description = '';
    if (address) {
      description = `${address}, `;
    }
    if (city) {
      description += `${city}, `;
    }
    if (state) {
      description += `${state}, `;
    }
    if (postalCode) {
      description += `${postalCode}, `;
    }
    if (country) {
      description += `${country}`;
    }

    return description;
  }
}
