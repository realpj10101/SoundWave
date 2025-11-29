import { AiRecommend } from "./ai-recommend.model";

export type Bubble = {
    role: 'user' | 'agent';
    text: string | undefined;
    meta?: Partial<AiRecommend>;
}
