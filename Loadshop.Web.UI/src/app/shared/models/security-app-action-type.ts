export enum SecurityAppActionType {
  // Admin Section
  ShipperAdminTabVisible = 'loadshop.ui.admin.tab.shippers.visible',
  CarrierAdminTabVisible = 'loadshop.ui.admin.tab.carriers.visible',
  ShipperCarrierGroupsAddEdit = 'loadshop.ui.system.shipper.carriergroups.addedit',
  ShipperAddEdit = 'loadshop.ui.system.shipper.addedit',
  CarrierAddEdit = 'loadshop.ui.system.carrier.addedit',
  SpecialInstructionsAddEdit = 'loadshop.ui.shipper.comments.addedit',
  CarrierUserAddEdit = 'loadshop.ui.system.carrier.user.addedit',
  ShipperUserAddEdit = 'loadshop.ui.system.shipper.user.addedit',
  AdminTabVisible = 'loadshop.ui.admin.tab.visible',
  UserCommunicationAddEdit = 'loadshop.ui.system.usercommunication.addedit',

  // Carrier actions
  ViewFavorites = 'loadshop.ui.profile.favorites.view',
  AddEditFavorites = 'loadshop.ui.profile.favorites.addedit',
  CarrierMarketPlaceView = 'loadshop.ui.marketplace.loads.view',
  CarrierMyLoadsView = 'loadshop.ui.myloads.viewbooked',
  CarrierViewStatus = 'loadshop.ui.myloads.status.view',
  CarrierUpdateStatus = 'loadshop.ui.myloads.status.update',
  ViewDetail = 'loadshop.ui.marketplace.loads.viewdetail',
  CarrierViewDelivered = 'loadshop.ui.carrier.myloads.viewdelivered',
  CarrierViewDeliveredDetail = 'loadshop.ui.carrier.myloads.viewdelivereddetail',

  // Shipper actions
  CreateManualLoad = 'loadshop.ui.shopit.load.manual.create',
  EditManualLoad = 'loadshop.ui.shopit.load.manual.edit',
  ShipperViewNewLoads = 'loadshop.ui.shopit.load.viewnew',
  ShipperViewBookedLoads = 'loadshop.ui.shopit.load.viewbooked',
  ShipperViewBookedDetail = 'loadshop.ui.shopit.load.viewbookeddetail',
  ShipperViewPostedLoads = 'loadshop.ui.shopit.load.viewposted',
  ShipperViewPostedDetail = 'loadshop.ui.shopit.load.viewposteddetail',
  ShipperViewDeliveredLoads = 'loadshop.ui.shopit.load.viewdelivered',
  ShipperViewDeliveredDetail = 'loadshop.ui.shopit.load.viewdelivereddetial',
  ShipperViewActiveLoads = 'loadshop.ui.shopit.load.viewactive',
  ShipperViewSmartSpotPriceQuote = 'loadshop.ui.shopit.smartspotprice.quote',
  ShipperProfileCustomerMapping = 'loadshop.ui.shipperprofile.customermapping',

  // User Profile
  LoadStatusNotifications = 'loadshop.ui.profile.loadstatusnotifications',

  // Fake
  Jabroni = 'loadshop.ui.jabroni',
}
