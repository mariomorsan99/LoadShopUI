export interface LoadStatusTransaction {
    loadId: string;
    messageId: string;
    messageTime: Date;
    transactionDtTm: Date;
}

export const defaultLoadStatusTransaction: LoadStatusTransaction = {
    loadId: null,
    messageId: null,
    messageTime: null,
    transactionDtTm: null,
};
