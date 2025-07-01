export interface Audio {
    uploaderName: string;
    fileName: string;
    filePath: string;
    uploadedAt: Date;
    isLiking: boolean;
    isAdding: boolean;
    likersCount: number;
    addersCount: number;
}