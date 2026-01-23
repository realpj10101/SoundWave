import {Member} from './member.model';

export interface CreateComment {
  content: string;
}

export interface CommentResponse {
  commentId: string;
  commentOwner: Member;
  content: string;
  createdAt: Date;
}
