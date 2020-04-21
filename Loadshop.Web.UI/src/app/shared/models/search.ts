export class Search {
  userLaneId?: string;
  origLat?: number;
  origLng?: number;
  origCity?: string;
  origState?: string;
  origCountry?: string;
  origDH?: number;
  destLat?: number;
  destLng?: number;
  destCity?: string;
  destState?: string;
  destCountry?: string;
  destDH?: number;
  origDateStart?: any;
  origDateEnd?: any;
  destDateStart?: any;
  destDateEnd?: any;
  equipString?: string;
  equipmentType?: string;
  searchType?: SearchTypeData;
  sortName?: string;
  sortDir?: 'asc' | 'desc';
  quickSearch?: string;
  serviceTypes?: number[];

  constructor() {
    this.searchType = 0;
  }
}

export enum SearchTypeData {
  OriginDest = 0,
  Statewide = 1,
  UserLanes = 2,
  Booked = 3,
  Delivered = 4,
}

export const defaultSearch: Search = {
  searchType: SearchTypeData.UserLanes,
  origLat: null,
  origLng: null,
  destLat: null,
  destLng: null,
  quickSearch: null,
  origDateStart: '',
  origDateEnd: '',
  destDateStart: '',
  destDateEnd: '',
  equipmentType: '',
  origDH: 50,
  destDH: 50,
  serviceTypes: null,
};
