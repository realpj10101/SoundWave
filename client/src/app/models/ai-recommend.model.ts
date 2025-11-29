import { Audio } from "./audio.model";

export interface AiRecommend {
    message?: string;
    items: Audio[];
}