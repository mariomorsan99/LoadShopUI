import { SpecialInstructionEquipmentData } from './special-instruction-equipment-data';

export interface SpecialInstructionData {
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
}

export const defaultSpecialInstructions: SpecialInstructionData = {
  specialInstructionId: null,
  name: null,
  description: null,
  comments: null,
  customerId: null,
  originAddress1: null,
  originCity: null,
  originState: null,
  originPostalCode: null,
  originCountry: null,
  destinationAddress1: null,
  destinationCity: null,
  destinationState: null,
  destinationPostalCode: null,
  destinationCountry: null,
  specialInstructionEquipment: [],
};
