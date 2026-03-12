import { MainPhoto } from "./photo.model";

export interface Audio {
    id: string;
    uploaderName: string;
    fileName: string;
    coverPath: MainPhoto;
    uploadedAt: Date;
    isLiking: boolean;
    isAdding: boolean;
    likersCount: number;
    addersCount: number;
    genres: [];
    moods: [];
    tags: [];
    duration: number;
}