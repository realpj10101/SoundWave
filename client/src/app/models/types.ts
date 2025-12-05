import { AiRecommend } from "./ai-recommend.model";

export type Bubble = {
    role: 'user' | 'agent';
    text: string | undefined;
    meta?: Partial<AiRecommend>;
}


export const GENRES = ['rock', 'pop', 'metal', 'electronic', 'hiphop', 'jazz'];
export const MOODS = ['energetic', 'dark', 'happy', 'calm', 'melancholic'];
