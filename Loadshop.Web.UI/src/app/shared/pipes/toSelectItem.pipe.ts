import { Pipe, PipeTransform } from '@angular/core';
import { SelectItem } from 'primeng/components/common/selectitem';
import {
  Carrier,
  Commodity,
  CustomerLoadType,
  Equipment,
  isCarrierArray,
  isCommodityArray,
  isCustomerLoadTypeArray,
  isEquipmentArray,
  isRatingQuestionArray,
  isUserAdminDataArray,
  RatingQuestion,
  UserAdminData,
} from '../models';

@Pipe({ name: 'toSelectItem' })
export class ToSelectItemPipe implements PipeTransform {
  public transform(list: Equipment[] | Carrier[] | Commodity[] | UserAdminData[] | CustomerLoadType[] | RatingQuestion[]): SelectItem[] {
    if (!list || !list.length) {
      return undefined;
    }
    if (isEquipmentArray(list)) {
      return list.map(p => ({
        label: p.equipmentDesc,
        value: p.equipmentId,
      }));
    } else if (isCarrierArray(list)) {
      return list.map(p => ({ label: p.carrierId, value: p.carrierId }));
    } else if (isCommodityArray(list)) {
      return list.map(p => ({
        label: p.commodityName,
        value: p.commodityName,
      }));
    } else if (isUserAdminDataArray(list)) {
      return list.map(p => ({
        label: `${p.firstName} ${p.lastName}`,
        value: p.userId,
      }));
    } else if (isCustomerLoadTypeArray(list)) {
      return list.map(p => ({
        label: p.name,
        value: p.customerLoadTypeId,
      }));
    } else if (isRatingQuestionArray(list)) {
      return list.map(p => ({
        label: p.question,
        value: p.ratingQuestionId,
      }));
    }
  }
}
