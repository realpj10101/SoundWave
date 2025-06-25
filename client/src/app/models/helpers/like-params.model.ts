import { PaginationParams } from "./paginationParams.model";

export class LikeParams extends PaginationParams {
    predicate: number = 0; // 0 => 'likings'; 1 => 'likers';
}