import { Carrier } from './carrier';
import { CarrierScac } from './carrier-scac';

export interface CarrierProfile extends Carrier {
    address: string;
    city: string;
    state: string;
    zip: string;
    country: string;
    usdotNbr: string;
    operatingAuthNbr: string;
    isLoadshopActive: boolean;
    rmisCertification: string;
    kbxlContracted: boolean;


    carrierSuccessTeamLeadId: string;
    carrierSuccessSpecialistId: string;
    comments: string;

    scacs: CarrierScac[];

}
