import { Photo } from "./photo.model";

export interface Member {
    userName: string;
    bio: string;
    photos: Photo[];
}