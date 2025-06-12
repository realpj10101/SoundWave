export interface Register {
    userName: string;
    email: string;
    password: string;
}

export interface Login {
    email: string;
    password: string;
}

export interface LoggedInUser {
    token: string;
    userName: string;
    roles: string[];
}