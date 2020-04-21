export interface Equipment {
    equipmentId: string;
    equipmentDesc: string;
    categoryId: string;
    categoryEquipmentDesc: string;
    categoryAbbr: string;
}

export const defaultEquipment: Equipment = {
    equipmentId: null,
    equipmentDesc: null,
    categoryId: null,
    categoryEquipmentDesc: null,
    categoryAbbr: null
};

export function isEquipment(x: any): x is Equipment {
    return (
        typeof x.equipmentDesc === 'string' && typeof x.equipmentId === 'string'
    );
}

export function isEquipmentArray(x: any): x is Equipment[] {
    return x.every(isEquipment);
}
